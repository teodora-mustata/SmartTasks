import { TestBed } from '@angular/core/testing';

import { Boards } from './boards';

describe('Boards', () => {
  let service: Boards;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Boards);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
