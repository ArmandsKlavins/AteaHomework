// The API answers every error with ProblemDetails; validation errors also carry an errors map
export interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}
