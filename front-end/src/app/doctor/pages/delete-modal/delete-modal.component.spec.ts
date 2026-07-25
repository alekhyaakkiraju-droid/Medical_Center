import { TestBed } from '@angular/core/testing';
import { DeleteModalComponent } from './delete-modal.component';

describe('DeleteModalComponent', () => {
  let component: DeleteModalComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [DeleteModalComponent]
    });
    component = TestBed.createComponent(DeleteModalComponent).componentInstance;
  });

  it('shows and hides using Angular state instead of Flowbite JS', () => {
    expect(component.isVisible).toBeFalse();

    component.showModal();
    expect(component.isVisible).toBeTrue();

    component.onCancel();
    expect(component.isVisible).toBeFalse();
  });

  it('emits confirm with the selected item id', () => {
    component.itemId = 42;
    component.showModal();

    let confirmedId: number | undefined;
    component.confirm.subscribe((id) => {
      confirmedId = id;
    });

    component.onConfirm();
    expect(confirmedId).toBe(42);
    expect(component.isVisible).toBeFalse();
  });
});
