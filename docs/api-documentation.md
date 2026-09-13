\# CoffeeNChill API Documentation



\## Menu Endpoints



\### 1. Create Menu Item

\- \*\*Method\*\*: POST

\- \*\*URL\*\*: `/api/menu`

\- \*\*Body\*\*:

\\`\\`\\`json

{

&#x20; "PartitionKey": "Hot Drinks",

&#x20; "RowKey": "COF-001",

&#x20; "Name": "Espresso",

&#x20; "Description": "Rich and bold Italian-style coffee",

&#x20; "Price": 3.50,

&#x20; "IsAvailable": true

}

\\`\\`\\`

\- \*\*Response\*\*: 201 Created



\### 2. Get All Menu Items

\- \*\*Method\*\*: GET

\- \*\*URL\*\*: `/api/menu`

\- \*\*Response\*\*: 200 OK with array of items



\### 3. Get by Category

\- \*\*Method\*\*: GET

\- \*\*URL\*\*: `/api/menu/category/{category}`

\- \*\*Example\*\*: `/api/menu/category/Hot%20Drinks`



\### 4. Update Menu Item

\- \*\*Method\*\*: PUT

\- \*\*URL\*\*: `/api/menu/{category}/{id}`

\- \*\*Body\*\*:

\\`\\`\\`json

{

&#x20; "Price": 4.00,

&#x20; "IsAvailable": false

}

\\`\\`\\`



\### 5. Delete Menu Item

\- \*\*Method\*\*: DELETE

\- \*\*URL\*\*: `/api/menu/{category}/{id}`

\- \*\*Response\*\*: 204 No Content



\## Document Endpoints



\### 1. Upload Staff Document

\- \*\*Method\*\*: POST

\- \*\*URL\*\*: `/api/documents/upload`

\- \*\*Body\*\*: form-data with key `file`

\- \*\*Response\*\*: 201 Created with metadata



\### 2. List Staff Documents

\- \*\*Method\*\*: GET

\- \*\*URL\*\*: `/api/documents`

\- \*\*Response\*\*: 200 OK with list of files



\### 3. Download Staff Document

\- \*\*Method\*\*: GET

\- \*\*URL\*\*: `/api/documents/download/{fileName}`

\- \*\*Response\*\*: File stream

