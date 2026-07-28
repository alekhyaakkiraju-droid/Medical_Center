import { Component, EventEmitter, HostListener, Input, Output, ChangeDetectionStrategy, inject } from '@angular/core';
import { A11yModule } from '@angular/cdk/a11y';
import { ModalFocusService } from '../../../shared/services/modal-focus.service';

@Component({
    selector: 'app-delete-modal',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './delete-modal.component.html',
    imports: [A11yModule],
})
export class DeleteModalComponent {
  @Input() itemId!: number;
  @Output() confirm = new EventEmitter<number>();

  isVisible = false;
  private readonly modalFocus = inject(ModalFocusService);

  showModal(trigger?: HTMLElement): void {
    this.modalFocus.open(trigger);
    this.isVisible = true;
  }

  hideModal(): void {
    this.isVisible = false;
    this.modalFocus.close();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isVisible) {
      this.onCancel();
    }
  }

  onCancel(): void {
    this.hideModal();
  }

  onConfirm(): void {
    this.hideModal();
    this.confirm.emit(this.itemId);
  }
}
