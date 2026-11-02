import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { catchError, finalize, map, of } from 'rxjs';
import { BillingApi } from './services/billing-api';
import { ProblemDetails } from './models/problem-details';
import { Receipt } from './models/receipt';

// The API only sends gateway IDs; display names are the client's job
const gatewayNames: Record<string, string> = { card: 'Card', bank: 'Bank transfer' };

@Component({
  selector: 'app-root',
  imports: [ReactiveFormsModule, DatePipe, DecimalPipe],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly api = inject(BillingApi);

  protected readonly submitting = signal(false);
  protected readonly receipt = signal<Receipt | null>(null);
  protected readonly error = signal<string | null>(null);

  protected readonly gateways = toSignal(
    this.api.getPaymentGateways().pipe(
      map((gateways) =>
        gateways
          .map((gateway) => ({ id: gateway.id, name: gatewayNames[gateway.id] ?? gateway.id }))
          .sort((a, b) => a.name.localeCompare(b.name)),
      ),
      catchError(() => {
        this.error.set("Couldn't load the payment gateways. Is the billing API running?");
        return of([]);
      }),
    ),
    { initialValue: [] },
  );

  // Mirrors the API's validation rules (OrderRequest), so most mistakes are caught before sending
  protected readonly form = inject(NonNullableFormBuilder).group({
    orderNumber: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^[A-Za-z0-9_-]+$/)]],
    userId: ['', [Validators.required, Validators.maxLength(100)]],
    payableAmount: [null as number | null, [Validators.required, Validators.min(0.01), Validators.pattern(/^\d+(\.\d{1,2})?$/)]],
    paymentGatewayId: ['', Validators.required],
    description: ['', Validators.maxLength(500)],
  });

  protected invalid(name: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[name];
    return control.invalid && control.touched;
  }

  protected generateOrderNumber(): void {
    this.form.controls.orderNumber.setValue(`ORD-${crypto.randomUUID().slice(0, 8).toUpperCase()}`);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.receipt.set(null);
    this.error.set(null);
    this.api
      .processOrder({ ...value, payableAmount: value.payableAmount!, description: value.description || undefined })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (receipt) => this.receipt.set(receipt),
        error: (response: HttpErrorResponse) => this.error.set(errorMessage(response)),
      });
  }
}

function errorMessage(response: HttpErrorResponse): string {
  if (response.status === 0) {
    return "Can't reach the billing API. Is it running?";
  }

  const problem = response.error as ProblemDetails | null;
  if (problem?.errors) {
    return Object.values(problem.errors).flat().join(' ');
  }

  return problem?.detail ?? problem?.title ?? 'Something went wrong. Please try again.';
}
