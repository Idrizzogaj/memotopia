export interface PaginationResultInterface<PaginationEntity> {
    results: PaginationEntity[];
    page: number;
    limit: number;
    total: number;
}
