﻿// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.Dtos;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;
using Tests.Helpers;

namespace Tests.UnitTests.DDDEntities
{
    public class TestDddOrder
    {
        [Fact]
        public void TestCreateOrderWithCorrectLineNumsOk()
        {
            //SETUP
            var book1 = DddEfTestData.CreateDummyBookOneAuthor();
            var book2 = DddEfTestData.CreateDummyBookOneAuthor();

            //ATTEMPT
            var bookOrders = new List<OrderBooksDto>() { new OrderBooksDto(book1.BookId, book1, 1), new OrderBooksDto(book2.BookId, book2, 2) };
            var order = new Order("user", DateTime.Today.AddDays(3), bookOrders, s => throw new Exception());

            //VERIFY
            order.LineItems.Count().ShouldEqual(2);
            order.LineItems.First().LineNum.ShouldEqual((byte)1);
            order.LineItems.Last().LineNum.ShouldEqual((byte)2);
        }

        [Fact]
        public void TestCreateOrderCorrectBookInfoOk()
        {
            //SETUP
            var book = DddEfTestData.CreateDummyBookOneAuthor();

            //ATTEMPT
            var lineItems = new List<OrderBooksDto> { new OrderBooksDto(book.BookId, book, 3) };
            var order = new Order("user", DateTime.Today.AddDays(3), lineItems, s => throw new Exception());

            //VERIFY
            order.LineItems.Count().ShouldEqual(1);
            order.LineItems.First().NumBooks.ShouldEqual((short)3);
            order.LineItems.First().BookPrice.ShouldEqual(book.ActualPrice);
        }

        [Fact]
        public void TestCreateOrderNoLineItemsOk()
        {
            //SETUP

            //ATTEMPT
            string errMessage = null;
            var order = new Order("user", DateTime.Today.AddDays(3), new OrderBooksDto[]{}, s => errMessage = s);

            //VERIFY
            errMessage.ShouldEqual("No items in your basket.");
        }

        [Fact]
        public void TestChangeDeliveryDateOk()
        {
            //SETUP
            var book = DddEfTestData.CreateDummyBookOneAuthor();
            var lineItems = new List<OrderBooksDto> { new OrderBooksDto(book.BookId, book, 3) };
            var order = new Order("user", DateTime.Today.AddDays(1), lineItems, s => throw new Exception());

            //ATTEMPT
            var newDeliverDate = DateTime.Today.AddDays(2);
            if (newDeliverDate.DayOfWeek == DayOfWeek.Sunday)
                newDeliverDate = newDeliverDate.AddDays(1);
            var status = order.ChangeDeliveryDate("user", newDeliverDate);

            //VERIFY
            status.HasErrors.ShouldBeFalse();
            order.ExpectedDeliveryDate.ShouldEqual(newDeliverDate);
        }

        [Fact]
        public void TestCreateOrderAndAddToDbOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
            }

            using (var context = new EfCoreContext(options))
            {
                //ATTEMPT
                var book = context.Books.First();
                var lineItems = new List<OrderBooksDto> { new OrderBooksDto(book.BookId, book, 1) };
                context.Add( new Order("user", DateTime.Today.AddDays(3), lineItems, s => throw new Exception()));
                context.SaveChanges();

                //VERIFY
                context.Orders.Count().ShouldEqual(1);
                context.Set<LineItem>().Count().ShouldEqual(1);
            }
        }

        

    }

}