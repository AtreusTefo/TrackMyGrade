export function extractErrors(error: any): string[] {
  // Network-level failure (backend not running, CORS blocked, etc.)
  if (error.status === 0) {
    return ['Cannot connect to the server. Please ensure the backend API is running on http://localhost:5000.'];
  }

  // Server returned a plain string body (e.g. BadRequest("Email already registered"))
  if (typeof error.error === 'string' && error.error.length > 0) {
    return [error.error];
  }

  // Server returned a validation object
  if (error.error && typeof error.error === 'object' && !(error.error instanceof ProgressEvent)) {
    const messages: string[] = [];

    // Web API ModelState format: { "Message": "...", "ModelState": { "field": ["err1"] } }
    const modelState = error.error.ModelState || error.error.modelState;
    if (modelState && typeof modelState === 'object') {
      for (const key of Object.keys(modelState)) {
        const val = modelState[key];
        if (Array.isArray(val)) {
          messages.push(...val.map((v: any) => String(v)));
        } else if (typeof val === 'string') {
          messages.push(val);
        }
      }
      if (messages.length > 0) return messages;
    }

    // Generic object format: { "FieldName": ["message1"], ... }
    for (const key of Object.keys(error.error)) {
      if (key === 'Message' || key === 'message' || key === 'ModelState' || key === 'modelState') continue;
      const val = error.error[key];
      if (Array.isArray(val)) {
        messages.push(...val.map((v: any) => String(v)));
      } else if (typeof val === 'string') {
        messages.push(val);
      }
    }
    if (messages.length > 0) return messages;

    // Fall back to the Message property
    const msg = error.error.Message || error.error.message;
    if (typeof msg === 'string' && msg.length > 0) {
      return [msg];
    }
  }

  // Fallback to the HTTP error message string
  if (error.message && typeof error.message === 'string') {
    return [error.message];
  }

  return ['An unexpected error occurred. Please try again.'];
}
