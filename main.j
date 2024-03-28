
void PrintInt(int n){
    printf("%d\n", n);
}

int Test(int a, int b){
    return a+b;
}

int main(){
    for(i, 0, 10){
        PrintInt(i*2);
    }
    int j=5;
    while(true){
        if(j>10){
            break;
        }
        PrintInt(Test(j, -3));
        j=j+1;
    } 
    return 0;
}