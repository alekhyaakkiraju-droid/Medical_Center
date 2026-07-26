import { TestBed } from '@angular/core/testing';
import { DeleteModalComponent } from '../doctor/pages/delete-modal/delete-modal.component';
import { PaymentComponent } from '../pages/general/Payment/Payment.component';
import { SideBarComponent } from '../admin/pages/side-bar/side-bar.component';
import { standaloneComponentTestProviders } from '../testing/standalone-component-test-providers';

describe('Shared standalone components', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: standaloneComponentTestProviders,
      imports: [DeleteModalComponent, PaymentComponent, SideBarComponent],
    }).compileComponents();
  });

  it('should compile shared standalone components without NgModules', () => {
    expect(TestBed.createComponent(DeleteModalComponent)).toBeTruthy();
    expect(TestBed.createComponent(PaymentComponent)).toBeTruthy();
    expect(TestBed.createComponent(SideBarComponent)).toBeTruthy();
  });
});
