using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ghostly.Data
{
    public static class ModelBuilderExtensions
    {
        public static EntityTypeBuilder<T> CaseInsensitive<T>(
            this EntityTypeBuilder<T> builder, params Expression<Func<T, string>>[] properties)
                where T : class
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            foreach (var property in properties)
            {
                builder.Property(property).HasColumnType("TEXT COLLATE NOCASE");
            }

            return builder;
        }
    }
}
