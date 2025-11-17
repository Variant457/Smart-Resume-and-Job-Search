import requests
import json

OLLAMA_URL = "http://localhost:11434/api/chat"
MODEL = "llama3.1:8b"

SYSTEM = "You are a precise resume editor. Never fabricate experience."

def chat(messages):
    """Send messages to Ollama and return raw text (streamed JSON lines)."""
    payload = {"model": MODEL, "messages": messages}
    r = requests.post(OLLAMA_URL, json=payload, timeout=180)
    r.raise_for_status()
    return r.text

def extract_json(stream_text):
    """
    Parse NDJSON from Ollama and extract assistant content.
    Then combine content into a single JSON string and parse it.
    """
    assistant_text = ""
    for line in stream_text.splitlines():
        if not line.strip():
            continue
        try:
            data = json.loads(line)
        except json.JSONDecodeError:
            continue  # skip malformed line
        msg = data.get("message")
        if msg and msg.get("role") == "assistant":
            content = msg.get("content", "")
            assistant_text += content

    # Now assistant_text should be a valid JSON string
    try:
        return json.loads(assistant_text)
    except json.JSONDecodeError as e:
        raise ValueError(f"Failed to parse JSON from assistant output: {e}\nContent:\n{assistant_text}")

def analyze_job(job_text):
    ANALYZE_INSTRUCTIONS = f"""
Return a single valid JSON object ONLY. Do NOT include any extra text.
Keys: job_summary, must_haves, nice_to_haves, keywords.
Job Description:
{job_text}
"""
    messages = [
        {"role": "system", "content": SYSTEM},
        {"role": "user", "content": ANALYZE_INSTRUCTIONS}
    ]
    raw = chat(messages)
    return extract_json(raw)

def rewrite_resume(resume_text, analysis):
    REWRITE_INSTRUCTIONS = f"""
Return a single valid JSON object ONLY. Do NOT include any extra text.
Use the job analysis below to tailor the resume.
Rules:
- Do NOT invent achievements or employers.
- Keep dates, employers, and titles unchanged.
- Rewrite bullets to be active, specific, outcome-focused.
- Add TODO markers if metrics or evidence are missing.
Output JSON keys: tailored_summary, experience, skills_section, gaps_and_questions

Job Analysis:
{json.dumps(analysis)}

Resume:
{resume_text}
"""
    messages = [
        {"role": "system", "content": SYSTEM},
        {"role": "user", "content": REWRITE_INSTRUCTIONS}
    ]
    raw = chat(messages)
    return extract_json(raw)

if __name__ == "__main__":
    with open("job.txt", encoding="utf-8") as f:
        job_text = f.read()
    with open("resume.txt", encoding="utf-8") as f:
        resume_text = f.read()

    analysis = analyze_job(job_text)
    tailored = rewrite_resume(resume_text, analysis)

    # Save output
    with open("tailored_resume.json", "w", encoding="utf-8") as f:
        json.dump({"analysis": analysis, "tailored": tailored}, f, indent=2, ensure_ascii=False)

    print("Tailored resume saved to 'tailored_resume.json'.")
