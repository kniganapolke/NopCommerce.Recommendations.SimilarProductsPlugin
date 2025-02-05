*Description*

Similar Products plugin brings capability to automatically discover and display similar products.
Similarity is a metric between 0 and 1 - the closer it is to 1, the more similar products are considered to be.
Similarity is calculated between products' properties using Cosine and Pearson similarity formulas provided by MathNet.Numerics library.
Data is preprocessed with Microsoft.ML library.
The plugin is displayed under "Recommendations" group.

*Requirements*

Microsoft.ML library relies on MICROSOFTML_RESOURCE_PATH environment variable to be set to a writable directory that application has access to.
This variable can be set from web.config:

<aspNetCore>
	<environmentVariables>
      <environmentVariable name="MICROSOFTML_RESOURCE_PATH" value="<PATH_TO_WRITABLE_DIRECTORY>" />
  </environmentVariables>
</aspNetCore>

*How to use*

After plugin has been installed, there is still no similar products discovered. This process has to be initiated manually.
It can be done either from configuration page using "Train Model" button or using a predefined scheduled task (Admin/ScheduleTask/List) which is disabled by default.

Plugin configuration page (Admin/RecommendationsSimilarProducts/Configure) provides some settings that can be changed:
 * Some product's properties can be excluded from algorithm, though all properties are included by default;
 * Number of similar products to find;
 * Number of similar products and to display in widget;
 * Similarity limit.

Plugin provides a widget that is displayed on Products Details page which can be managed from Widgets page (Admin/Widget/List).
The widget is disabled by default.

Widget's title reads "Similar Products" by default. One can change it through admin interface (Configuration-Languages-String Resources)
searching by the key - "Plugins.Recommendations.SimilarProducts.SimilarProducts".

Similarity value for each product displayed in "Similar Products" widget can be found by inspecting HTML markup of each product.
Look for hidden element <h3 class="similarity"></h3>.

*License*

Plugin is distributed under MIT License.
