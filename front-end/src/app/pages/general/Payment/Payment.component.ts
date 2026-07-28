import { Component, EventEmitter, HostListener, OnDestroy, OnInit, Output, ChangeDetectionStrategy, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { A11yModule } from '@angular/cdk/a11y';
import { ModalFocusService } from '../../../shared/services/modal-focus.service';


@Component({
    selector: 'app-Payment',
    templateUrl: './Payment.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./Payment.component.css'],
    imports: [ReactiveFormsModule, A11yModule]
})

export class PaymentComponent implements OnInit, OnDestroy {
  checkoutForm: FormGroup;
  private readonly modalFocus = inject(ModalFocusService);

  constructor(
    private fb: FormBuilder,

  ) {
    this.checkoutForm = this.fb.group({
      cardHolderName: ['', [Validators.required, Validators.minLength(3)]],
      postalCode: ['', [Validators.required]],
      cardNumber: ['', [Validators.required]],
      expiryDate: ['', [Validators.required]],
      cvv: ['', [Validators.required]],
    });
  }

  get cardHolderName() { return this.checkoutForm.get('cardHolderName'); }
  get postalCode() { return this.checkoutForm.get('postalCode'); }
  get cardNumber() { return this.checkoutForm.get('cardNumber'); }
  get expiryDate() { return this.checkoutForm.get('expiryDate'); }
  get cvv() { return this.checkoutForm.get('cvv'); }

  ngOnInit() {
    this.modalFocus.open();
  }

  ngOnDestroy(): void {
    this.modalFocus.close();
  }

  @Output() paymentSuccess = new EventEmitter<boolean>(); 
  @Output() close = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closeModal();
  }

  closeModal() {
    this.modalFocus.close();
    this.close.emit();
  }

  onSubmitPayment(event: Event) {
    event.preventDefault();
    if (this.checkoutForm.valid) {
      this.paymentSuccess.emit(true); 
    } else {
      this.paymentSuccess.emit(false); 
    }
  }
}
