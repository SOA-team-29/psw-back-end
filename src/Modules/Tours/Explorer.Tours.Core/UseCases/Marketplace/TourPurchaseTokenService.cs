﻿using AutoMapper;
using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Administration;
using Explorer.Tours.API.Public.Marketplace;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using Explorer.Tours.Core.Domain.ShoppingCarts;
using Explorer.Tours.Core.Domain.Tours;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.Core.UseCases.Marketplace
{
    public class TourPurchaseTokenService : CrudService<TourPurchaseTokenDto, TourPurchaseToken>, ITourPurchaseTokenService
    {
        private readonly ICrudRepository<Tour> _tourRepository;
        private readonly IMapper _mapper;
        private readonly ITourRepository _tRepository;
        public TourPurchaseTokenService(ICrudRepository<TourPurchaseToken> crudRepository, IMapper mapper, ICrudRepository<Tour> tourRepository, ITourRepository tRepository) : base(crudRepository, mapper)
        {
            _tourRepository = tourRepository;
            _tRepository = tRepository;
            _mapper = mapper;
        }

        public Result<List<TourDTO>> GetPurchasedTours(int touristId)
        {
            //dobaviti sve tokene
            var tokens = CrudRepository.GetPaged(1, int.MaxValue).Results.Where(token => token.TouristId == touristId);
            //za svaki token izbaciti njegovu turu
            var purchacedTours = new List<TourDTO>();
            foreach(var token in tokens) 
            {
                Tour tour = _tRepository.GetByTourId(token.IdTour);
                TourDTO tourDto = _mapper.Map<TourDTO>(tour);
                purchacedTours.Add(tourDto);
            }
            //dodati u enum stanje koje proverava da li je poceta ili ne ( ili kako vec treba)
            return purchacedTours;

        }
    }
}