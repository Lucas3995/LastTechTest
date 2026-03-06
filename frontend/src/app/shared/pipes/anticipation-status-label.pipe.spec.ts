// Plano árvore testes 4 ajustes frontend — T3: pipe status → label amigável

import { AnticipationStatusLabelPipe } from './anticipation-status-label.pipe';
import { AnticipationRequestStatus } from '../../domain';

describe('AnticipationStatusLabelPipe', () => {
  const pipe = new AnticipationStatusLabelPipe();

  it('should transform Pending to "Em analise"', () => {
    expect(pipe.transform(AnticipationRequestStatus.Pending)).toBe('Em analise');
  });

  it('should transform Approved to "Aprovada"', () => {
    expect(pipe.transform(AnticipationRequestStatus.Approved)).toBe('Aprovada');
  });

  it('should transform Rejected to "Recusada"', () => {
    expect(pipe.transform(AnticipationRequestStatus.Rejected)).toBe('Recusada');
  });

  it('should transform CanceledByCreator to "Cancelada pelo creator"', () => {
    expect(pipe.transform(AnticipationRequestStatus.CanceledByCreator)).toBe('Cancelada pelo creator');
  });

  it('should return empty string for null', () => {
    expect(pipe.transform(null)).toBe('');
  });

  it('should return empty string for undefined', () => {
    expect(pipe.transform(undefined)).toBe('');
  });

  it('should return string representation for unknown value (fallback)', () => {
    expect(pipe.transform('UNKNOWN' as AnticipationRequestStatus)).toBe('UNKNOWN');
  });
});
