Episerver Find & api.ai Proof of Concept 
===========
This repository is a fork of Episerver Quicksilver 10.2.0 which demonstrates a proof of concept for integrating Api.ai Natural Language Processing into Episerver Find Commerce Search. The search functionality in Quicksilver has been converted to Episerver Find, and has been modified to pre-filter Find facets based on api.ai parsing of natural language queries.

api.ai integration is handled by https://github.com/api-ai/api-ai-net

Installation
------------

1.  Follow the Episerver Quicksilver installation instructions at https://github.com/episerver/Quicksilver
2.  Read up on the api.ai documentation to understand features and functionality
2.  Create an Episerver Find Developer index, and add the associated index configuration in the Web.config of the Commerce.Site project
2.  Create an api.ai Account, and acquire an API key, adding it to the AppSettings section of the Web.config: <add key="api-ai-key" value="key-goes-here"/>
3.  Upload the "color", "brand", "gender" and "category" entity json exports from EPiServer.Reference.Commerce.Site\Features\Search\ApiAiExports to api.ai
4.  Upload the "search-by-category" intent json export from EPiServer.Reference.Commerce.Site\Features\Search\ApiAiExports to api.ai
5.  Run the site
6.  Update the "tees" node in the Catalog to be "shirts-w" (necessary to align the category codes within the example catalog data)
4.  Run the Episerver Find indexing job to index all product data
5.  On the Search Page, check the box for "Enable AI Search?" and publish the page
6.  Search for utterances defined in the "search-by-category" intent

Future Considerations
------------
* Using api.ai entity parsing to boost certain fields in search requests instead of pre-faceting search results
* Using api.ai intents to detect specific thematic searches and subsequently redirect the user to specific content within Episerver
* Using api.ai to adjust search result layouts based on intent (for example, showing specific marketing content by brand)

License
-------------
[![License](http://img.shields.io/:license-apache-blue.svg?style=flat-square)](http://www.apache.org/licenses/LICENSE-2.0.html)