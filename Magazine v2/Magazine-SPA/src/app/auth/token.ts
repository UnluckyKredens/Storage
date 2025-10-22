export function hasValidToken(token: string | null): boolean {
  if (!token) {
    return false;
  }

  try {
    const encodedPayload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const payload = JSON.parse(atob(encodedPayload)) as { exp?: number };
    return typeof payload.exp === 'number' && payload.exp * 1000 > Date.now();
  } catch {
    return false;
  }
}
