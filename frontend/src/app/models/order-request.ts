export interface OrderRequest {
  orderNumber: string;
  userId: string;
  payableAmount: number;
  paymentGatewayId: string;
  description?: string;
}
