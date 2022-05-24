SELECT Id, CAST(ProductTypeId AS REAL) AS ProductTypeId, CAST(ParentGroupedProductId AS REAL) AS ParentGroupedProductId, CAST(VendorId AS REAL) AS VendorId, CAST(Price AS REAL) AS Price, RequireOtherProducts, 
                  RequiredProductIds, IsDownload, IsRecurring, CAST(Weight AS REAL) AS Weight, CAST(Length AS REAL) AS Length, CAST(Width AS REAL) AS Width, CAST(Height AS REAL) AS Height, Name, MetaKeywords, MetaTitle, ShortDescription, 
                  FullDescription,
                      (SELECT string_agg(pc.CategoryId, ',') AS Categories
                       FROM      dbo.Category AS c LEFT OUTER JOIN
                                         dbo.Product_Category_Mapping AS pc ON pc.CategoryId = c.Id AND pc.ProductId = p.Id) AS Categories,
                      (SELECT string_agg(pm.ManufacturerId, ',') AS Manufacturers
                       FROM      dbo.Manufacturer AS m LEFT OUTER JOIN
                                         dbo.Product_Manufacturer_Mapping AS pm ON pm.ManufacturerId = m.Id AND pm.ProductId = p.Id) AS Manufacturers,
                      (SELECT string_agg(pam.ProductAttributeId, ',') AS ProductAttributes
                       FROM      dbo.ProductAttribute AS pa LEFT OUTER JOIN
                                         dbo.Product_ProductAttribute_Mapping AS pam ON pam.ProductAttributeId = pa.Id AND pam.ProductId = p.Id) AS ProductAttributes,
					   (SELECT string_agg(ptm.ProductTag_Id, ',') AS ProductTags
                       FROM      dbo.ProductTag AS pa LEFT OUTER JOIN
                                         dbo.Product_ProductTag_Mapping AS ptm ON ptm.ProductTag_Id = pa.Id AND ptm.Product_Id = p.Id) AS ProductTags
FROM     dbo.Product AS p
WHERE  (Deleted = 0)