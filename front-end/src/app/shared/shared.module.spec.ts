import { TestBed } from '@angular/core/testing';
import { SharedModule } from './shared.module';
import { DeleteModalComponent } from '../doctor/pages/delete-modal/delete-modal.component';
import { PaymentComponent } from '../pages/general/Payment/Payment.component';
import { SideBarComponent } from '../admin/pages/side-bar/side-bar.component';

describe('SharedModule', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SharedModule],
    }).compileComponents();
  });

  it('should compile shared components', () => {
    expect(TestBed.createComponent(DeleteModalComponent)).toBeTruthy();
    expect(TestBed.createComponent(PaymentComponent)).toBeTruthy();
    expect(TestBed.createComponent(SideBarComponent)).toBeTruthy();
  });
});
