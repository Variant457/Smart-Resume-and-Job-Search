import json
import requests
import sys
import traceback


class ParsingModel:
    def __init__(self):
        self.ollamaHost = "http://localhost:11434"
        self.model = "llama3.1:8b"

    def call_ollama(self, prompt: str) -> str:
        print("[ParsingModel] Calling Ollama...", file=sys.stderr, flush=True)

        try:
            response = requests.post(
                f"{self.ollamaHost}/api/generate",
                json={
                    "model": self.model,
                    "prompt": prompt,
                    "stream": False,
                },
                timeout=60,
            )

            print("[ParsingModel] Ollama responded, status:", response.status_code, file=sys.stderr, flush=True)

            response.raise_for_status()
            data = response.json()
            return data.get("response", "")
        except Exception as e:
            print("[ParsingModel] Error calling Ollama:", repr(e), file=sys.stderr, flush=True)

            traceback.print_exc(file=sys.stderr)
            return json.dumps({
                "error": f"Failed to call Ollama model: {str(e)}",
            })

    def build_prompt(self, resume: str) -> str:
        schema = """
You are a resume parsing engine.

Extract structured data from the resume below and output ONLY a single JSON object
with exactly the following keys:

{
  "name": string | null,
  "location": string | null,
  "emails": string[] | null,          // all emails found (primary first)
  "phones": string[] | null,          // all phones found (primary first)

  "profiles": [          // online profiles (LinkedIn, portfolio, GitHub, etc.)
    {
      "type": string | null,      // e.g. "linkedin", "portfolio", "github", "website", "other"
      "url": string | null,
      "label": string | null      // e.g. "LinkedIn", "Portfolio", "Behance"
    }
  ] | null,

  "headline": string | null,      // short title: "Full-stack Developer", "Registered Nurse"
  "summary": string | null,       // free-form summary / objective paragraph

  "skills": {
    "by_category": [
      {
        "category": string | null,  // e.g. "Programming Languages", "Sales", "Teaching"
        "items": string[] | null        // e.g. ["C#", "JavaScript"] or ["Prospecting", "Negotiation"]
      }
    ],
    "keywords": string[] | null         // flattened skill keywords for search / matching
  } | null,

  "experience": [
    {
      "job_title": string | null,
      "organization": string | null,    // company, school, hospital, etc.
      "location": string | null,
      "start_date": string | null,      // prefer "YYYY-MM" or "YYYY"
      "end_date": string | null,        // "present" if still employed
      "employment_type": string | null,     // e.g. "full_time", "part_time", "contract", "internship", "volunteer"
      "description": string | null,         // paragraph description, if any
      "highlights": string[] | null,       // bullet points / key achievements
      "skills": string[] | null            // skills/tech/keywords inferred from this role
    }
  ] | null,

  "education": [
    {
      "degree": string | null,          // "B.S.", "MBA", "High School Diploma"
      "field_of_study": string | null,  // "Computer Science", "Marketing"
      "institution": string | null,
      "location": string | null,
      "start_date": string | null,
      "end_date": string | null,
      "graduation_year": string | null
    }
  ] | null,

  "certifications": [
    {
      "name": string | null,
      "issuer": string | null,
      "issue_date": string | null,
      "expiry_date": string | null,
      "credential_id": string | null
    }
  ] | null,

  "projects": [
    {
      "name": string | null,
      "role": string | null,          // "Developer", "Research Assistant", etc.
      "description": string | null,
      "highlights": string[] | null,
      "skills": string[] | null       // tech or domain skills used
    }
  ] | null,

  "awards": [
    {
      "name": string | null,
      "issuer": string | null,
      "year": string | null,
      "description": string | null
    }
  ] | null,

  "publications": [
    {
      "title": string | null,
      "venue": string | null,       // journal, conference, blog, etc.
      "year": string | null,
      "url": string | null
    }
  ] | null,

  "languages": [
    {
      "name": string | null,            // "English", "Spanish"
      "proficiency": string | null      // e.g. "native", "professional", "basic"
    }
  ] | null,

  "interests": string[] | null          // free form: ["climbing", "urban photography", ...]
}


Rules:
- Always include every key shown above, even if the value is null.
- Use null where the information is missing or cannot be confidently inferred.
- Use arrays ([]) for list fields, even if they contain only one item.
- Do NOT include any comments, explanations, or extra keys.
- Do NOT wrap the JSON in markdown or any additional text.
"""
        return f"""{schema}

Resume text:
\"\"\"{resume}\"\"\"

Return ONLY the JSON object. Remember: no surrounding markdown, no extra commentary.
"""

def main():
    print("[ParsingModel] Reading stdin...", file=sys.stderr, flush=True)
    text = sys.stdin.read()
    print("[ParsingModel] Read input, length:", len(text), file=sys.stderr, flush=True)
    if not text.strip():
        print("{}", end="")
        return

    parser = ParsingModel()
    print("[ParsingModel] Building prompt...", file=sys.stderr, flush=True)
    prompt = parser.build_prompt(text)
    print("[ParsingModel] Prompt built, length:", len(prompt), file=sys.stderr, flush=True)
    raw_response = parser.call_ollama(prompt).strip()
    print("[ParsingModel] Raw response received, length:", len(raw_response), file=sys.stderr, flush=True)

    start = raw_response.find("{")
    end = raw_response.rfind("}")
    if start != -1 and end != -1 and start < end:
        raw_json = raw_response[start : end + 1]
    else:
        raw_json = raw_response

    try:
        parsed = json.loads(raw_json)
    except json.JSONDecodeError:
        parsed = {
            "error": "Failed to parse JSON from model",
            "raw_response": raw_response,
        }

    print(json.dumps(parsed, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
