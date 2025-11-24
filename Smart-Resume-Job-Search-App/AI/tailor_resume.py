import sys
import json
import requests
import re

# Ollama config
OLLAMA_URL = "http://localhost:11434/api/chat"
MODEL = "llama3.1:8b"

SYSTEM = (
    "You are a precise resume editor. You take an existing resume and rewrite it "
    "so it is tailored for a specific job description. You never invent new employers, "
    "dates, or roles that are not present in the original resume, but you may rephrase, "
    "reorder, and condense content to better match the job."
)

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

def _main():
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

        user_prompt = f"""
You will be given a job description and an existing resume.

Rewrite the resume so that it is strongly tailored to this job.

Constraints:
- Do NOT invent employers, job titles, dates, or degrees that are not present in the original resume.
- You may rephrase bullets, reorder sections, and highlight the most relevant experience.
- Use clear section headings (Summary, Experience, Skills, Education, etc.).
- Use '-' bullet points where appropriate.
- Return ONLY the full rewritten resume as plain text. Do NOT return JSON, XML, code fences, or explanations.

Job description:
{job_text}

Original resume:
{resume_text}
"""

        messages = [
            {"role": "system", "content": SYSTEM},
            {"role": "user", "content": user_prompt},
        ]

        tailored_resume = chat(messages).strip()
        if not tailored_resume:
            raise ValueError("Model returned empty content")

        sys.stdout.write(tailored_resume)
        sys.stdout.flush()

    except Exception as ex:
        # Errors go to stderr so C# code can detect failure
        print(f"error: {ex}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    _main()