import { HttpParams } from '@angular/common/http';

export function toHttpParams(
  record: Record<string, string | number | boolean | null | undefined>
): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(record)) {
    if (value === null || value === undefined) {
      continue;
    }
    if (typeof value === 'string' && value.trim() === '') {
      continue;
    }
    params = params.set(key, String(value));
  }
  return params;
}
