import { h } from "preact";
import * as S from "./login.styled";
import * as FS from "../../style/fonts";
import * as GS from "../../style/style";

const login = () => {
    alert("send email");
    return
}

const Login = () => {


    return (
        <S.Wrapper>
            <S.Content>
                <FS.Title>Login</FS.Title>
                <GS.Break theme={GS.mediumBreak} />
                <S.LoginForm>
                    <S.TextSpan>
                        <FS.SubTitle>Email</FS.SubTitle>
                        <GS.Break theme={GS.smallBreak} />
                        <S.EmailInput/>
                    </S.TextSpan>
                </S.LoginForm>
                <GS.Break theme={GS.mediumBreak} />
                <S.SendMailButton onClick={login}>Send mail</S.SendMailButton>
            </S.Content>
        </S.Wrapper>
    );
};

export default Login;