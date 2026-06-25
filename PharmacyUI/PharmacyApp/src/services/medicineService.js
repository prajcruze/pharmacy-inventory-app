const BASE = import.meta.env.VITE_API_BASE_URL;

// helper — reads the error message from a failed response
async function readError(res) {
  try {
    const body = await res.json();
    return body?.error ?? `Request failed (${res.status})`;
  } catch {
    return `Request failed (${res.status})`;
  }
}

export async function getMedicines(search = '') {
  const url = search
    ? `${BASE}/medicines?search=${encodeURIComponent(search)}`
    : `${BASE}/medicines`;

  const res = await fetch(url);
  if (!res.ok) throw new Error(await readError(res));
  return res.json();
}

export async function addMedicine(medicine) {
  const res = await fetch(`${BASE}/medicines`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(medicine),
  });
  if (!res.ok) throw new Error(await readError(res));
  return res.json();
}