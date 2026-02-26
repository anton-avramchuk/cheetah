export interface JwtPayload {
  sub:          string;
  email?:       string;
  unique_name?: string;
  role?:        string | string[];
  exp:          number;
  [key: string]: unknown;
}

export interface ParsedUser {
  id:          string;
  displayName: string;
  email:       string;
  roles:       string[];
}

export function parseJwt(token: string): ParsedUser {
  try {
    const payload = JSON.parse(atob(token.split('.')[1])) as JwtPayload;
    const rawRoles = payload['role'];
    const roles = Array.isArray(rawRoles)
      ? rawRoles
      : rawRoles ? [rawRoles] : [];

    return {
      id:          String(payload['sub'] ?? ''),
      displayName: String(payload['unique_name'] ?? payload['email'] ?? 'User'),
      email:       String(payload['email'] ?? ''),
      roles,
    };
  } catch {
    return { id: '', displayName: 'User', email: '', roles: [] };
  }
}

export function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1])) as JwtPayload;
    return Date.now() >= Number(payload['exp']) * 1000;
  } catch {
    return true;
  }
}
