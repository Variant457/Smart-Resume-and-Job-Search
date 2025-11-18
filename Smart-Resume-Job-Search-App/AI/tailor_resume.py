import sys
import json
import requests

# Ollama config
OLLAMA_URL = "http://localhost:11434/api/chat"
MODEL = "llama3.1:8b"

SYSTEM = "You are a precise resume editor. Never fabricate experience."


def chat(messages):
    """
    Send messages to Ollama and return the assistant's content text.

    We explicitly set stream=False so the API returns a single JSON object.
    """
    payload = {
        "model": MODEL,
        "messages": messages,
        "stream": False
    }
    r = requests.post(OLLAMA_URL, json=payload, timeout=180)
    r.raise_for_status()
    resp = r.json()
    content = resp.get("message", {}).get("content", "")
    return content


def extract_json(text: str):
    """
    Extract the first JSON object from the model's text output.
    The model is instructed to return only JSON, but this is a safety net.
    """
    text = text.strip()
    # Fast path: try direct parse
    try:
        return json.loads(text)
    except json.JSONDecodeError:
        pass

    # Fallback: find the first {...} block
    start = text.find("{")
    end = text.rfind("}")
    if start == -1 or end == -1 or end <= start:
        raise ValueError(f"No JSON object found in model output.\nRaw content:\n{text}")

    json_str = text[start:end+1]
    return json.loads(json_str)


def analyze_job(job_text: str):
    """
    Ask Ollama to analyze the job description and return a JSON object with:
      - job_summary
      - must_haves
      - nice_to_haves
      - keywords
    """
    ANALYZE_INSTRUCTIONS = f"""
Return a single valid JSON object ONLY. Do NOT include any extra text.

The JSON must have these keys:
- job_summary: one-paragraph summary of the role.
- must_haves: array (8–15 items) of REQUIRED skills/experience.
- nice_to_haves: array of nice-to-have skills/experience.
- keywords: array of exact phrases useful for ATS.

Job Description:
{job_text}
"""
    messages = [
        {"role": "system", "content": SYSTEM},
        {"role": "user", "content": ANALYZE_INSTRUCTIONS}
    ]
    raw = chat(messages)
    return extract_json(raw)


def rewrite_resume(resume_text: str, analysis: dict):
    """
    Ask Ollama to tailor the resume to the job analysis.

    Expected JSON keys in response:
      - tailored_summary  (string)
      - experience        (array of rewritten experience entries/bullets)
      - skills_section    (array of skills/phrases)
      - gaps_and_questions (array of TODOs / missing info notes)
    """
    REWRITE_INSTRUCTIONS = f"""
Return a single valid JSON object ONLY. Do NOT include any extra text.

Use the job analysis below to tailor the resume.

Rules:
- Do NOT invent achievements, employers, or tools that do not appear in the resume.
- Keep dates, employers, and titles unchanged.
- Rewrite bullets to be active, specific, outcome-focused.
- Prefer wording from the job description where it is truthful.
- Add "TODO:" markers where metrics or evidence are missing.
- Output JSON with keys:
    tailored_summary,
    experience,
    skills_section,
    gaps_and_questions

Job Analysis:
{json.dumps(analysis, ensure_ascii=False)}

Resume:
{resume_text}
"""
    messages = [
        {"role": "system", "content": SYSTEM},
        {"role": "user", "content": REWRITE_INSTRUCTIONS}
    ]
    raw = chat(messages)
    return extract_json(raw)


def _main():
    """
    CLI entry point.

    Expects a single JSON object on stdin:

    {
      "resume": ".... full resume text ....",
      "job": ".... full job description text ...."
    }

    Writes a single JSON object to stdout:

    {
      "analysis": { ... },
      "tailored": { ... }
    }
    """
    try:
        raw_input = sys.stdin.read()
        if not raw_input.strip():
            raise ValueError("No input received on stdin. Expected JSON with 'resume' and 'job' fields.")

        data = json.loads(raw_input)

        # Accept a few alternative field names defensively
        resume_text = (
            data.get("resume")
            or data.get("resume_text")
            or data.get("Resume")
            or data.get("ResumeText")
        )
        job_text = (
            data.get("job")
            or data.get("job_text")
            or data.get("jobDescription")
            or data.get("Job")
            or data.get("JobText")
        )

        if not resume_text or not job_text:
            raise ValueError("Expected JSON with 'resume' and 'job' (or 'resume_text' / 'job_text') fields.")

        analysis = analyze_job(job_text)
        tailored = rewrite_resume(resume_text, analysis)

        output = {
            "analysis": analysis,
            "tailored": tailored
        }

        # IMPORTANT: output ONLY JSON to stdout
        json.dump(output, sys.stdout, ensure_ascii=False)
        sys.stdout.flush()

    except Exception as ex:
        # Errors go to stderr so C# code can detect failure
        print(f"error: {ex}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    _main()
