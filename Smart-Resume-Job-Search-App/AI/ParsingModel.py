from transformers import pipeline
import sys;
import json;

class ResumeParser:
    def __init__(self):
        self.model_name="yashpwr/resume-ner-bert-v2"

    def parse(self, text):
        ner = pipeline(
            "token-classification",
            model=self.model_name,
            aggregation_strategy="simple"
        )

        return ner(text)

def main():
    text = sys.stdin.read()
    parser = ResumeParser()
    raw_entities = parser.parse(text)
    
    cleaned = []
    for ent in raw_entities:
        start = int(ent["start"])
        end = int(ent["end"])
        cleaned.append({
            "start": start,
            "end": end,
            "text": text[start:end],
            "label": ent.get("entity_group") or ent.get("entity"),  # depending on transformer version
            "score": float(ent["score"]),
        })

    print(json.dumps(cleaned, ensure_ascii=False))

if __name__ == "__main__":
    main()