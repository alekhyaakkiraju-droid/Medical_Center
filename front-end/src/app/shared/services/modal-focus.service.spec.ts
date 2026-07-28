import { TestBed } from '@angular/core/testing';
import { ModalFocusService } from './modal-focus.service';

describe('ModalFocusService', () => {
  let service: ModalFocusService;
  let mainContent: HTMLElement;
  let trigger: HTMLButtonElement;

  beforeEach(() => {
    mainContent = document.createElement('main');
    mainContent.id = 'main-content';
    document.body.appendChild(mainContent);

    trigger = document.createElement('button');
    document.body.appendChild(trigger);
    trigger.focus();

    TestBed.configureTestingModule({
      providers: [ModalFocusService],
    });
    service = TestBed.inject(ModalFocusService);
  });

  afterEach(() => {
    mainContent.remove();
    trigger.remove();
  });

  it('marks main content aria-hidden while modal is open', () => {
    service.open(trigger);
    expect(mainContent.getAttribute('aria-hidden')).toBe('true');
  });

  it('restores focus to trigger when modal closes', () => {
    service.open(trigger);
    service.close();
    expect(document.activeElement).toBe(trigger);
    expect(mainContent.hasAttribute('aria-hidden')).toBeFalse();
  });
});
