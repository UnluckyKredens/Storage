# ApiEndpoints
url: localhost:3000
# Item 
//Find All items
url: GET localhost:3000/item

//Find One item by Id / sku
url: GET gelocalhost:3000/item?id=1
url: GET localhost:3000/item?sku=""

//Add Item
url: POST localhost:3000/item 
Body:
{
  "name" : "",
  "sku": "",
  "categoryId": 1,
  "bruttoPrice" : 1,
  "nettoPrice" : 1
}

//Update Item
url: url: PATCH, localhost:3000/item
Body:
{
  "id" : 1
  "name" : "",
  "sku": "",
  "categoryId": 1,
  "bruttoPrice" : 1,
  "nettoPrice" : 1
}
//Delete Item
url: DELETE, localhost:3000/item/ 





# Category 
//Find All
url: GET localhost:3000/category

//Find One
url: GET localhost:3000/category?id=1
url: GET localhost:3000/category?name="a"

//Add Category
url: POST localhost:3000/category/add
Body: 
{
    "name" ""
}

//Update Category
url: PATCH localhost:3000/category
Body:
{
    "id": 1,
    "name" : ""
}

//Delete category
url: DELETE localhost:3000/category




# Stock 

//Find All
url: GET localhost:3000/stock

//Find One by ItemId
url: GET localhost:3000/stock/item/1

//Add to stock (new row or increase existing)
url: POST localhost:3000/stock
Body:
{
  "itemId": 1,
  "quantity": 5
}

//Update stock row
url: PATCH localhost:3000/stock
Body:
{
  "id": 10,
  "quantity": 12
}

//Delete stock row
url: DELETE localhost:3000/stock
Body:
{
  "id": 10
}

//Clean table
url: POST localhost:3000/stock/clean