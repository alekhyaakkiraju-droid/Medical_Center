import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-delete-modal',
  templateUrl: './delete-modal.component.html'
})
export class DeleteModalComponent {
  @Input() itemId!: number;
  @Output() confirm = new EventEmitter<number>();

  isVisible = false;

  showModal(): void {
    this.isVisible = true;
  }

  hideModal(): void {
    this.isVisible = false;
  }

  onCancel(): void {
    this.hideModal();
  }

  onConfirm(): void {
    this.hideModal();
    this.confirm.emit(this.itemId);
  }
}
