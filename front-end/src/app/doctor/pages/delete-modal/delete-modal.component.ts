import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-delete-modal',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './delete-modal.component.html',
    imports: [],
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
