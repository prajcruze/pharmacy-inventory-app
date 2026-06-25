const BASE = import.meta.env.VITE_API_BASE_URL;

async function readError(res) {
  try {
    const body = await res.json();
    return body?.error ?? `Request failed (${res.status})`;
  } catch {
    return `Request failed (${res.status})`;
  }
}

export async function getSales() {
  const res = await fetch(`${BASE}/sales`);
  if (!res.ok) throw new Error(await readError(res));
  return res.json();
}

export async function recordSale(sale) {
  const res = await fetch(`${BASE}/sales`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(sale),
  });
  if (!res.ok) throw new Error(await readError(res));
  return res.json();
}