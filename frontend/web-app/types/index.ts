export type PagedResult<T> = {
  results: T[];
  pageCount: number;
  totalCount: number;
};

export type Auction = {
  id: string;
  reservePrice: number;
  seller: string;
  winner?: string;
  soldAmount: number;
  currentHighBid: number;
  auctionEnd: string;
  createdAt: string;
  updatedAt: string;
  status: string;
  make: string;
  model: string;
  year: number;
  color: string;
  mileage: number;
  imageUrl: string;
};
