using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nop.Plugin.Widgets.FacebookPixel.Extension
{
    public static class ControllerExtensions
    {
        public static string PreparePrice(this Product product, IWorkContext workContext, IStoreContext storeContext, IProductService productService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, IPermissionService permissionService, ILocalizationService localizationService, ITaxService taxService, ICurrencyService currencyService)
        {
            decimal taxRate = new decimal();
            //decimal taxRate = new decimal();
            string str;
            bool flag;
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }
            string price = "0";
            ProductType productType = product.ProductType;
            if (productType != ProductType.SimpleProduct)
            {
                if (productType != ProductType.GroupedProduct)
                {
                   // goto Label2;
                }
                IList<Product> associatedProducts = productService.GetAssociatedProducts(product.Id, storeContext.CurrentStore.Id, 0, false);
                if (associatedProducts.Count == 0)
                {
                    price = "0";
                }
                else if (!permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                {
                    price = "0";
                }
                else
                {
                    decimal? minPossiblePrice = null;
                    Product minPriceProduct = null;
                    foreach (Product associatedProduct in associatedProducts)
                    {
                        decimal tmpPrice = priceCalculationService.GetFinalPrice(associatedProduct, workContext.CurrentCustomer, decimal.Zero, true, 2147483647);
                        if ((!minPossiblePrice.HasValue ? true : tmpPrice < minPossiblePrice.Value))
                        {
                            minPriceProduct = associatedProduct;
                            minPossiblePrice = new decimal?(tmpPrice);
                        }
                    }
                    if ((minPriceProduct == null ? false : !minPriceProduct.CustomerEntersPrice))
                    {
                        if (minPriceProduct.CallForPrice)
                        {
                            price = "0";
                        }
                        else if (!minPossiblePrice.HasValue)
                        {
                            Debug.WriteLine("Cannot calculate minPrice for product #{0}", new object[] { product.Id });
                        }
                        else
                        {
                            decimal finalPriceBase = taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, out taxRate);
                            decimal finalPrice = currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, workContext.WorkingCurrency);
                            price = string.Concat(finalPrice);
                        }
                    }
                }
                str = price;
                return str;
            }
            if (!permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                price = "0";
            }
            else if (!product.CustomerEntersPrice)
            {
                if (!product.CallForPrice)
                {
                    decimal minPossiblePrice = priceCalculationService.GetFinalPrice(product, workContext.CurrentCustomer, decimal.Zero, true, 2147483647);
                    decimal oldPriceBase = taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                    decimal finalPriceBase = taxService.GetProductPrice(product, minPossiblePrice, out taxRate);
                    currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, workContext.WorkingCurrency);
                    decimal finalPrice = currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, workContext.WorkingCurrency);
                    List<TierPrice> tierPrices = new List<TierPrice>();
                    if (product.HasTierPrices)
                    {
                        tierPrices.AddRange(TierPriceExtensions.RemoveDuplicatedQuantities(TierPriceExtensions.FilterForCustomer(TierPriceExtensions.FilterByStore((
                            from tp in product.TierPrices
                            orderby tp.Quantity
                            select tp).ToList<TierPrice>(), storeContext.CurrentStore.Id), workContext.CurrentCustomer)));
                    }
                    if (tierPrices.Count <= 0)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = (tierPrices.Count != 1 ? true : tierPrices[0].Quantity > 1);
                    }
                    if (!flag)
                    {
                        price = ((finalPriceBase == oldPriceBase ? true : oldPriceBase == decimal.Zero) ? string.Concat(finalPrice) : string.Concat(finalPrice));
                    }
                    else
                    {
                        price = string.Concat(finalPrice);
                    }
                }
                else
                {
                    price = "0";
                }
            }
            str = price;
            return str;
        }
    }
}
