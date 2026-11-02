import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { OrderRequest } from '../models/order-request';
import { PaymentGateway } from '../models/payment-gateway';
import { Receipt } from '../models/receipt';

// shortcut: hardcoded API URL, move it to configuration when the API runs anywhere but localhost:5209
export const apiUrl = 'http://localhost:5209/api';

@Injectable({ providedIn: 'root' })
export class BillingApi {
  private readonly http = inject(HttpClient);

  getPaymentGateways(): Observable<PaymentGateway[]> {
    return this.http.get<PaymentGateway[]>(`${apiUrl}/payment-gateways`);
  }

  processOrder(order: OrderRequest): Observable<Receipt> {
    return this.http.post<Receipt>(`${apiUrl}/orders`, order);
  }
}
