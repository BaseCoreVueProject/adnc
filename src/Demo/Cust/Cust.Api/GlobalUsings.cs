﻿global using System.Linq.Expressions;
global using System.Net;
global using System.Reflection;
global using System.Text;
global using Adnc.Demo.Const;
global using Adnc.Demo.Const.Permissions.Cust;
global using Adnc.Demo.Cust.Api.Application.Dtos;
global using Adnc.Demo.Cust.Api.Application.Subscribers;
global using Adnc.Demo.Cust.Api.Repository;
global using Adnc.Demo.Cust.Api.Repository.Entities;
global using Adnc.Infra.EventBus;
global using Adnc.Infra.IdGenerater.Yitter;
global using Adnc.Infra.Repository;
global using Adnc.Infra.Repository.EfCore;
global using Adnc.Shared;
global using Adnc.Shared.Application.Contracts.Attributes;
global using Adnc.Shared.Application.Contracts.Dtos;
global using Adnc.Shared.Application.Contracts.Interfaces;
global using Adnc.Shared.Application.Contracts.ResultModels;
global using Adnc.Shared.Application.Extensions;
global using Adnc.Shared.Application.Registrar;
global using Adnc.Shared.Application.Services;
global using Adnc.Shared.Application.Services.Trackers;
global using Adnc.Shared.Repository.EfCoreEntities;
global using Adnc.Shared.WebApi;
global using Adnc.Shared.WebApi.Authorization;
global using Adnc.Shared.WebApi.Registrar;
global using AutoMapper;
global using DotNetCore.CAP;
global using FluentValidation;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
