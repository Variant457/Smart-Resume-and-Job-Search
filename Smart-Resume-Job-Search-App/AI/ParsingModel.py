from transformers import AutoTokenizer, AutoModelForTokenClassification
import torch
import sys
import json
import os

class ResumeParser:
    def __init__(self, model_name="yashpwr/resume-ner-bert-v2"):
        self.tokenizer = AutoTokenizer.from_pretrained(model_name)
        self.model = AutoModelForTokenClassification.from_pretrained(model_name)
        self.model.eval()

    def parse(self, text: str):

        # Tokenize
        inputs = self.tokenizer(
            text,
            return_tensors="pt",
            truncation=True,
            max_length=512,
            padding=True
        )

        # Predict
        with torch.no_grad():
            outputs = self.model(**inputs)
            predictions = torch.argmax(outputs.logits, dim=2)

        # Extract entities
        entities = []
        current_entity = None

        input_ids = inputs['input_ids'][0]
        for i, pred in enumerate(predictions[0]):
            label = self.model.config.id2label[pred.item()]
            token = self.tokenizer.convert_ids_to_tokens(int(input_ids[i]))

            # basic wordpiece join: merge subwords starting with '##'
            token_clean = token.replace("##", "")

            if label.startswith('B-'):
                if current_entity:
                    entities.append(current_entity)
                current_entity = {
                    'text': token_clean,
                    'label': label[2:], # Remove 'B-' prefix
                    'start': i
                }
            elif label.startswith('I-') and current_entity:
                current_entity['text'] += ' ' + token_clean
            elif label == 'O':
                if current_entity:
                    entities.append(current_entity)
                    current_entity = None

        if current_entity:
            entities.append(current_entity)

        return entities


def _main():
    # CLI entry: read full stdin as text, output JSON to stdout
    try:
        text = sys.stdin.read()
        parser = ResumeParser()
        entities = parser.parse(text)
        # Output ONLY JSON to stdout (tools may parse it). Debug to stderr.
        json.dump(entities, sys.stdout, ensure_ascii=False)
        sys.stdout.flush()
    except Exception as ex:
        # Print a minimal error to stderr and exit non-zero
        print(f"error: {ex}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    _main()