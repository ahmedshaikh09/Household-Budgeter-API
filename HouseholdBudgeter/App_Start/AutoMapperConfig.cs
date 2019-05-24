using AutoMapper;
using HouseholdBudgeter.Models;
using HouseholdBudgeter.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.App_Start
{
    public static class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<HouseHold, HouseHoldViewModel>().ReverseMap();
                cfg.CreateMap<HouseHold, HouseHoldBindingModel>().ReverseMap();
                cfg.CreateMap<Category, CategoryViewModel>().ReverseMap();
                cfg.CreateMap<Category, CategoryBindingModel>().ReverseMap();
                cfg.CreateMap<BankAccount, BankAccountViewModel>().ReverseMap();
                cfg.CreateMap<BankAccount, BankAccountBindingModel>().ReverseMap();
                cfg.CreateMap<Transaction, TransactionViewModel>().ReverseMap();
                cfg.CreateMap<Transaction, TransactionBindingModel>().ReverseMap();
            });
        }
    }

}