import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { NppContent, NppService } from '../../services/npp.service';
import { NppModalRequest, NppModalService } from '../../services/npp-modal.service';

@Component({
  selector: 'app-npp-acknowledgment',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './npp-acknowledgment.component.html',
  imports: [],
})
export class NppAcknowledgmentComponent implements OnInit, OnDestroy {
  @Output() acknowledged = new EventEmitter<void>();

  isVisible = false;
  content: NppContent | null = null;

  private subscription?: Subscription;
  private pendingResolve: ((value: boolean) => void) | null = null;

  constructor(
    private nppModalService: NppModalService,
    private nppService: NppService
  ) {}

  ngOnInit(): void {
    this.subscription = this.nppModalService.request$.subscribe((request) => {
      this.open(request);
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  onAcknowledge(): void {
    this.close(true);
  }

  onDismiss(): void {
    this.close(false);
  }

  private open(request: NppModalRequest): void {
    this.content = request.content;
    this.pendingResolve = request.resolve;
    this.isVisible = true;
  }

  private close(result: boolean): void {
    this.isVisible = false;
    this.content = null;
    this.pendingResolve?.(result);
    this.pendingResolve = null;
  }
}
