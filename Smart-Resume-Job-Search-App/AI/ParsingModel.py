from transformers import AutoTokenizer, AutoModelForTokenClassification
import torch
import json
import sys

class ResumeParser:
    def __init__(self, model_name="yashpwr/resume-ner-bert-v2"):
        self.tokenizer = AutoTokenizer.from_pretrained(model_name)
        self.model = AutoModelForTokenClassification.from_pretrained(model_name)

    def parse(self, text):
        # Tokenize
        inputs = self.tokenizer(
            text,
            return_tensors="pt",
            truncation=True,
            max_length=128,
            padding=True
        )

        # Predict
        with torch.no_grad():
            outputs = self.model(**inputs)
            predictions = torch.argmax(outputs.logits, dim=2)

        input_ids = inputs['input_ids'][0].tolist()
        tokens = self.tokenizer.convert_ids_to_tokens(input_ids)

        # Extract entities
        entities = []
        current_entity = None

        for i, pred in enumerate(predictions[0]):
            label = self.model.config.id2label[pred.item()]
            token = tokens[i]
    
            if label.startswith('B-'):
                if current_entity:
                    entities.append(current_entity)
                current_entity = {
                    'text': token,
                    'label': label[2:],  # Remove 'B-' prefix
                    'start': i
                }
            elif label.startswith('I-') and current_entity:
                current_entity['text'] += ' ' + token
            elif label == 'O':
                if current_entity:
                    entities.append(current_entity)
                    current_entity = None

        if current_entity:
            entities.append(current_entity)

        return entities

def main():
    text = sys.stdin.read()
    parser = ResumeParser()
    entities = parser.parse(text)
    print(json.dumps(entities))

if __name__ == "__main__":
    main()