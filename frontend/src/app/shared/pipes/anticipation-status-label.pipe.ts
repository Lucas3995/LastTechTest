import { Pipe, PipeTransform } from '@angular/core';
import { AnticipationRequestStatus } from '../../domain';

const STATUS_LABELS: Record<AnticipationRequestStatus, string> = {
  [AnticipationRequestStatus.Pending]: 'Em analise',
  [AnticipationRequestStatus.Approved]: 'Aprovada',
  [AnticipationRequestStatus.Rejected]: 'Recusada',
  [AnticipationRequestStatus.CanceledByCreator]: 'Cancelada pelo creator',
};

@Pipe({ name: 'anticipationStatusLabel', standalone: true })
export class AnticipationStatusLabelPipe implements PipeTransform {
  transform(status: AnticipationRequestStatus | null | undefined): string {
    if (status == null) return '';
    return STATUS_LABELS[status] ?? String(status);
  }
}
