import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DeleteModalComponent } from './delete-modal.component';
import { ModalFocusService } from '../../../shared/services/modal-focus.service';

describe('DeleteModalComponent', () => {
  let component: DeleteModalComponent;
  let fixture: ComponentFixture<DeleteModalComponent>;
  let modalFocus: jasmine.SpyObj<ModalFocusService>;

  beforeEach(async () => {
    modalFocus = jasmine.createSpyObj('ModalFocusService', ['open', 'close']);

    await TestBed.configureTestingModule({
      imports: [DeleteModalComponent],
      providers: [{ provide: ModalFocusService, useValue: modalFocus }],
    }).compileComponents();

    fixture = TestBed.createComponent(DeleteModalComponent);
    component = fixture.componentInstance;
    component.itemId = 42;
    fixture.detectChanges();
  });

  it('should not be visible initially', () => {
    expect(component.isVisible).toBeFalse();
  });

  it('opens focus trap when showModal is called', () => {
    component.showModal();
    expect(component.isVisible).toBeTrue();
    expect(modalFocus.open).toHaveBeenCalled();
  });

  it('returns focus when modal is cancelled', () => {
    component.showModal();
    component.onCancel();
    expect(component.isVisible).toBeFalse();
    expect(modalFocus.close).toHaveBeenCalled();
  });

  it('closes on escape key when visible', () => {
    component.showModal();
    component.onEscape();
    expect(modalFocus.close).toHaveBeenCalled();
  });
});
