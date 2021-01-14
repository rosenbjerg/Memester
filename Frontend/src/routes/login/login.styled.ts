import styled from "styled-components";

export const Wrapper = styled("div")`
    height: 100%;
    width: 100%;
    display: grid;
    place-items: center;
`;
export const Content = styled("div")`
padding: 70px 0;
  display: flex;
  flex-flow: column;
  place-items: center;
  width: 500px; 
  background-color: var(--themeBlue); 
`;

export const TextSpan = styled("span")`
  color: var(--themeWhite);
`;

export const LoginForm = styled("div")`
  text-align: center;
  width: 100%;
`;

export const EmailInput = styled.input`
  
  height: 64px;
  width: calc(100% - 64px);
  -webkit-appearance: none;
  border: none;
  background-color: var(--themeBlack);
  &:focus{
  outline: none;
  }
`;

export const SendMailButton = styled.button`
    display: inline-block;
    color: var(--themeWhite);
    border: var(--themeWhite) solid 1px;
    font-size: 24px;
    padding: 8px;
    &:hover{
        cursor: pointer;
    color: var(--themeBlue);
    border: var(--themeWhite) solid 1px;
    background-color: var(--themeWhite);
   }
  &:active{
      border: var(--themeWhite) solid 1px;
    background-color: var(--themeWhite);
  }    
`;