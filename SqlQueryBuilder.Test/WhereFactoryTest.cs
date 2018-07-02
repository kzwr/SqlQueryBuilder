﻿using SqlQueryBuilder.Test.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SqlQueryBuilder.Test
{
    public class WhereFactoryTest
    {
        private static Dictionary<string, Type> GetMapper()
        {
            return new Dictionary<string, Type>()
            {
                {"Car", typeof(Car) },
                {"CarMaker", typeof(CarMaker) }
            };
        }

        [Fact]
        public void Or_WithinMapper_Valid()
        {
            Dictionary<string, Type> mapper = GetMapper();

            var builder = new WhereFactory(mapper).Or(
                CheapCarCondition,
                SweetSpotLexusCondition
            );

            var expectedWhereClause = $"(((([Car].[Mileage]) < ({CHEAPCAR_MILEAGE})) AND (([Car].[Price]) < ({CHEAPCAR_PRICE})))"
                + $" OR ((([Car].[ModelYear]) > ({LEXUS_YEAR})) AND (([Car].[Mileage]) < ({LEXUS_MILEAGE})) AND"
                + $" (([Car].[Price]) <= ({LEXUS_PRICE})) AND (([CarMaker].[Name]) LIKE ({LEXUS_BRAND}))))";

            Assert.True(builder.TryBuild(out var whereClause));
            Assert.True(whereClause == expectedWhereClause);
        }

        [Fact]
        public void Or_NotWithinMapper_Invalid()
        {
            var mapper = GetMapper();
            Assert.True(mapper.Where(x => x.Value == typeof(Country)).Count() == 0, "Country should not be in the mapper");

            IWhereBuilder CheapNonAmericanCondition(IWhereBuilderFactory factory) => factory.And(
                CheapCarCondition,
                // FAIL CONDITION: The "Country" table is not in the mapper
                f => f.Compare<Country>(country => country.Name, Compare.NEQ, "USA")
             );

            var builder = new WhereFactory(mapper).Or(
                CheapNonAmericanCondition,
                SweetSpotLexusCondition
            );

            Assert.False(builder.TryBuild(out var whereClause));
        }

        private const string CHEAPCAR_MILEAGE = "100000";
        private const string CHEAPCAR_PRICE = "5000";

        private IWhereBuilder CheapCarCondition(IWhereBuilderFactory factory)
        {
            return factory.And(
                f => f.Compare<Car>(car => car.Mileage, Compare.LT, CHEAPCAR_MILEAGE),
                f => f.Compare<Car>(car => car.Price, Compare.LT, CHEAPCAR_PRICE)
            );
        }

        private const string LEXUS_YEAR = "2015";
        private const string LEXUS_MILEAGE = "25000";
        private const string LEXUS_PRICE = "32000";
        private const string LEXUS_BRAND = "LEXUS";

        private IWhereBuilder SweetSpotLexusCondition(IWhereBuilderFactory factory)
        {
            return factory.And(
                f => f.Compare<Car>(car => car.ModelYear, Compare.GT, LEXUS_YEAR),
                f => f.Compare<Car>(car => car.Mileage, Compare.LT, LEXUS_MILEAGE),
                f => f.Compare<Car>(car => car.Price, Compare.LTE, LEXUS_PRICE),
                f => f.Compare<CarMaker>(maker => maker.Name, Compare.LIKE, LEXUS_BRAND)
            );
        }

    }
}