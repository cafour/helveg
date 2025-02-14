// https://stackoverflow.com/questions/54983096/defining-the-inverse-of-partialt-in-typescript
export type Full<T> = {
    [P in keyof T]-?: T[P];
};
