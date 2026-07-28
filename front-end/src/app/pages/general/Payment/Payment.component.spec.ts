import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { PaymentComponent } from './Payment.component';
import { ModalFocusService } from '../../../shared/services/modal-focus.service';

describe('PaymentComponent', () => {
  let component: PaymentComponent;
  let fixture: ComponentFixture<PaymentComponent>;
  let modalFocus: jasmine.SpyObj<ModalFocusService>;

  beforeEach(async () => {
    modalFocus = jasmine.createSpyObj('ModalFocusService', ['open', 'close']);

    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, PaymentComponent],
      providers: [{ provide: ModalFocusService, useValue: modalFocus }],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PaymentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('opens modal focus trap on init and closes on destroy', () => {
    expect(modalFocus.open).toHaveBeenCalled();
    fixture.destroy();
    expect(modalFocus.close).toHaveBeenCalled();
  });

  it('closes modal and returns focus when closeModal is called', () => {
    component.closeModal();
    expect(modalFocus.close).toHaveBeenCalled();
  });

  it('closes modal when escape is pressed', () => {
    component.onEscape();
    expect(modalFocus.close).toHaveBeenCalled();
  });
});
