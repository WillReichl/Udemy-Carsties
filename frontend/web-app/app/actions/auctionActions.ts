'use server';

import { PagedResult, Auction } from '@/types';
import { getTokenWorkaround } from './authActions';

export async function getData(query: string): Promise<PagedResult<Auction>> {
  const res = await fetch(`http://localhost:6001/search${query}`);
  if (!res.ok) throw new Error('Failed to fetch data');

  return res.json();
}

export async function UpdateAuctionTest() {
  const data = {
    mileage: Math.floor(Math.random() * 100000),
  };

  const token = await getTokenWorkaround();

  const result = await fetch(
    'http://localhost:6001/auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c',
    {
      method: 'PUT',
      headers: {
        'Content-type': 'application/json',
        Authorization: 'Bearer ' + token?.access_token,
      },
      body: JSON.stringify(data),
    }
  );

  if (!result.ok) return { status: result.status, message: result.statusText };

  return result.statusText;
}
