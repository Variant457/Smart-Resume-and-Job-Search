from transformers import AutoTokenizer, AutoModelForTokenClassification
import torch

class ResumeParser:
    def __init__(self, model_name="yashpwr/resume-ner-bert-v2"):
        self.tokenizer = AutoTokenizer.from_pretrained(model_name)
        self.model = AutoModelForTokenClassification.from_pretrained(model_name)

    def parse(self, text):
        # Tokenize
        inputs = tokenizer(
            text,
            return_tensors="pt",
            truncation=True,
            max_length=128,
            padding=True
        )

        # Predict
        with torch.no_grad():
            outputs = model(**inputs)
            predictions = torch.argmax(outputs.logits, dim=2)

        # Extract entities
        entities = []
        current_entity = None

        for i, pred in enumerate(predictions[0]):
            label = model.config.id2label[pred.item()]
            token = tokenizer.convert_ids_to_tokens(inputs['input_ids'][0][i])
    
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

        print("Extracted Entities:")
        for entity in entities:
            print(f"- {entity['label']}: {entity['text']}")