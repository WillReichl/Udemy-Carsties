'use client';

import React, { useEffect, useState } from 'react';
import AuctionCard from './AuctionCard';
import AppPagination from '../components/AppPagination';
import { Auction } from '@/types';
import { getData } from '../actions/auctionActions';
import Filters from './Filters';
import { pages } from 'next/dist/build/templates/app-page';

export default function Listings() {
  const [auctions, setAuctions] = useState<Auction[]>([]);
  const [pageCount, setPageCount] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(4);

  useEffect(() => {
    getData(pageNumber, pageSize).then((data) => {
      setAuctions(data.results);
      setPageCount(data.pageCount);
    });
  }, [pageNumber, pageSize]);

  if (auctions.length === 0) return <h3>Loading...</h3>;

  return (
    <>
      <Filters pageSize={pageSize} setPageSize={setPageSize} />
      <div className='grid grid-cols-4 gap-6'>
        {auctions.map((auction) => (
          <AuctionCard auction={auction} key={auction.id} />
        ))}
      </div>
      <div className='flex justify-center mt-4'>
        <AppPagination
          currentPage={pageNumber}
          pageCount={pageCount}
          pageChanged={setPageNumber}
        />
      </div>
    </>
  );
}
