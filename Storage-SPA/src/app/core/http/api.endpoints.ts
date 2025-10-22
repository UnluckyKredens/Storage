import { environment } from "../../../environments/environment";

export const apiEndpoints = {
   category: {
      categoryUrl: `${environment.apiUrl}/category`,
      // getOneCategory: `${environment.apiUrl}/category`
   },
   item: {
    itemUrl: `${environment.apiUrl}/item`,
   },
   stock: {
    stockUrl: `${environment.apiUrl}/stock`
   },
   purchase: {
    purchaseUrl: `${environment.apiUrl}/purchase`
   },
   seller: {
    sellerUrl: `${environment.apiUrl}/seller`
   },
   statistics: {
    statisticsUrl: `${environment.apiUrl}/statistics`,
    all: `${environment.apiUrl}/statistics/all`
   }
}
