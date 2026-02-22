export function extractErrors(error: any): string[] {
  // Network-level failure (backend not running, CORS blocked, etc.)
  if (error.status === 0) {
    return ['Cannot connect to the server. Please ensure the backend API is running on http://localhost:5000.'];
  }

  // Server returned a plain string body (e.g. BadRequest("Email already registered"))
  if (typeof error.error === 'string' && error.error.length > 0) {
    return [error.error];
  }

  // Server returned a validation object (e.g. BadRequest(ModelState))
  // Shape: { "FieldName": ["message1", "message2"], ... }
  if (error.error && typeof error.error === 'object' && !(error.error instanceof ProgressEvent)) {
    const messages: string[] = [];
    for (const key of Object.keys(error.error)) {
      const val = error.error[key];
      if (Array.isArray(val)) {
        messages.push(...val.map((v: any) => String(v)));
      } else if (typeof val === 'string') {
        messages.push(val);
      }
    }
    if (messages.length > 0) return messages;
  }

  // Fallback to the HTTP error message string
  if (error.message && typeof error.message === 'string') {
    return [error.message];
  }

  return ['An unexpected error occurred. Please try again.'];
}
