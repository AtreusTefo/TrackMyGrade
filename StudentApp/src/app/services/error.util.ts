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

export interface FieldErrorResult {
  fieldErrors: { [key: string]: string };
  generalErrors: string[];
}

/**
 * Extracts per-field errors from a Web API ModelState response and separates
 * them from general (non-field) errors. Used for inline form validation display.
 */
export function extractFieldErrors(error: any): FieldErrorResult {
  const fieldErrors: { [key: string]: string } = {};
  const generalErrors: string[] = [];

  if (error.status === 0) {
    generalErrors.push('Cannot connect to the server. Please ensure the backend API is running on http://localhost:5000.');
    return { fieldErrors, generalErrors };
  }

  if (typeof error.error === 'string' && error.error.length > 0) {
    generalErrors.push(error.error);
    return { fieldErrors, generalErrors };
  }

  if (error.error && typeof error.error === 'object' && !(error.error instanceof ProgressEvent)) {
    const modelState = error.error.ModelState || error.error.modelState;
    if (modelState && typeof modelState === 'object') {
      for (const key of Object.keys(modelState)) {
        const fieldName = normalizeFieldKey(key);
        const val = modelState[key];
        const msg = Array.isArray(val) ? String(val[0]) : String(val);
        if (msg) fieldErrors[fieldName] = msg;
      }
      if (Object.keys(fieldErrors).length > 0) return { fieldErrors, generalErrors };
    }

    const msg = error.error.Message || error.error.message;
    if (typeof msg === 'string' && msg.length > 0) {
      generalErrors.push(msg);
      return { fieldErrors, generalErrors };
    }
  }

  const fallback = error.message && typeof error.message === 'string'
    ? error.message
    : 'An unexpected error occurred. Please try again.';
  generalErrors.push(fallback);
  return { fieldErrors, generalErrors };
}

/** Strips parameter prefixes (e.g. "request.FirstName" → "firstName") */
function normalizeFieldKey(key: string): string {
  const name = key.includes('.') ? key.split('.').pop()! : key;
  return name.charAt(0).toLowerCase() + name.slice(1);
}
