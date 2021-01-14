import styled from "styled-components";

export const smallBreak = 16;
export const mediumBreak = 32;

export const Title = styled("span")`
  text-align: center;    
`;

export const Break = styled.div`
  width: 100%;
  height: 1px;
  margin-top: ${props => props.theme}px;
`

export const ScaleBreak = styled.div`
  width: 100%;
  height: 1px;
  margin-top: calc(${props => props.theme/2}px + 5vw);
      @media(min-width: 1330px){
        display: inline-block;
        margin-top: calc(${props => props.theme/2}px + 65px);
    }
`