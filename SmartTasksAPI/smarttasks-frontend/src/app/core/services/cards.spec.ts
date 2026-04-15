import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';

import { Cards } from './cards';

describe('Cards', () => {
  let service: Cards;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient()]
    });
    service = TestBed.inject(Cards);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
